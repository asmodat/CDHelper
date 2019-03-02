using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;

namespace CDHelper.Processing
{
    public static class DockerfileConfigGenerator
    {
        public static string GetDockerCompose(string imageName, int port, int[] exposedPorts, string[] portsMap)
        {
            if (exposedPorts.IsNullOrEmpty())
                exposedPorts = new int[] { port };
             
            if (!exposedPorts.Contains(port))
                exposedPorts = exposedPorts.Merge(port);

            exposedPorts = exposedPorts.Distinct();

            var expose = "";
            var ports = "";

            foreach (var p in exposedPorts)
            {
                if(p <= 0 || p > 65535)
                    continue;

                expose += $"      - \"{p}\"\r\n";
                ports += $"      - \"{p}:{p}\"\r\n";
            }

            expose = expose.TrimEnd("\r\n");
            ports = ports.TrimEnd("\r\n");

            if (!portsMap.IsNullOrEmpty())
            {
                portsMap = portsMap.Distinct();

                ports = "";
                foreach (var pm in portsMap)
                {
                    if(pm.IsNullOrEmpty() || !pm.Contains(":"))
                        continue;

                    var portMap = pm.Split(':');

                    if (portMap.IsNullOrEmpty() || portMap.Length != 2)
                        continue;

                    var p1 = portMap[0].ToIntOrDefault(-1);
                    var p2 = portMap[1].ToIntOrDefault(-1);

                    if (p1 < 1 || p2 < 1 || p1 > 65535 || p2 > 65535)
                        continue;

                    ports += $"      - \"{p1}:{p2}\"\r\n";
                }
                ports = ports.TrimEnd("\r\n");
            }

            return $@"version: '3.3'

services:
  app:
    container_name: ""{imageName}-container""
    image: {imageName}
    build:
      context: ./
      dockerfile: Dockerfile
    env_file:
      - ./.env
    expose:
{expose}
    ports:
{ports}";
        }

        public static string GetDockerfile(
            string baseImage, 
            string workingDirectory, 
            int port,
            int[] exposedPorts,
            string buildpackId, 
            bool enginx,
            string customBuildCommand,
            string customStartCommand,
            string[] env)
        {
            if (exposedPorts.IsNullOrEmpty())
                exposedPorts = new int[] { port };

            if (!exposedPorts.Contains(port))
                exposedPorts = exposedPorts.Merge(port);

            exposedPorts = exposedPorts.Distinct();

            var expose = "";
            var ports = "";

            foreach (var p in exposedPorts)
            {
                if (p <= 0 || p > 65535)
                    continue;

                expose += $"{p} ";
                ports += $"{p},";
            }
            expose = expose.TrimEnd(" ");
            ports = ports.TrimEnd(",");

            var envs = "";
            if (!env.IsNullOrEmpty())
            {
                foreach (var e in env)
                {
                    var l = e.Trim();
                    if (!l.IsNullOrEmpty())
                        envs += $"ENV {l}\r\n";
                }
            }

            workingDirectory = "/" + workingDirectory.TrimStart("/");

            return $@"
#base image, e.g.: settlefinance/deployment:1.0
#stack-images: https://github.com/SettleFinance/stack-images/
FROM {baseImage} as base

ENV PORT={port}
ENV PORTS={ports}
ENV NPM_CONFIG_PRODUCTION=false 
ENV YARN_PRODUCTION=false
ENV DANGEROUSLY_DISABLE_HOST_CHECK=true

#environment variables override:
{envs}

#setting up workingDirectory, e.g.: /app
COPY . {workingDirectory}
WORKDIR {workingDirectory}

#buildpackId, e.g.: nodejs, ruby, go, php, java, static
#more details: https://devcenter.heroku.com/articles/nodejs-support#package-installation
{(buildpackId.IsNullOrEmpty() ? string.Empty : "RUN mkdir -p /tmp/build_cache /tmp/env")}
{(buildpackId.IsNullOrEmpty() ? string.Empty : $"RUN STACK=heroku-16 /etc/buildpacks/heroku-buildpack-{buildpackId}/bin/compile {workingDirectory} /tmp/build_cache /tmp/env")}

#enginx configuration:
{(enginx ? "RUN rm -v -f /etc/nginx/nginx.conf" : string.Empty)}
{(enginx ? "ADD nginx.conf /etc/nginx/" : string.Empty)}

#custom build command:
{(customBuildCommand.IsNullOrEmpty() ? string.Empty : "RUN " + customBuildCommand)}

EXPOSE {expose}

CMD {(enginx ? "service nginx start &&" : "")} {(customStartCommand.IsNullOrEmpty() ? $"heroku local -p {port}" : customStartCommand)}
";
        }
    }
}
