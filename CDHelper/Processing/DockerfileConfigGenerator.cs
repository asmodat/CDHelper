using System;
using System.Collections.Generic;
using System.Text;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;

namespace CDHelper.Processing
{
    public static class DockerfileConfigGenerator
    {
        public static string GetDockerCompose(string imageName, int port)
        {
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
      - ""{port}""
    ports:
      - ""{port}:{port}""";
        }

        public static string GetDockerfile(
            string baseImage, 
            string workingDirectory, 
            int port, 
            string buildpackId, 
            bool enginx,
            string customBuildCommand,
            string customStartCommand,
            string[] env)
        {

            
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

#custom build command:
{(customBuildCommand.IsNullOrEmpty() ? string.Empty: "RUN " + customBuildCommand)}

#enginx configuration:
{(enginx ? "RUN rm -v -f /etc/nginx/nginx.conf" : string.Empty)}
{(enginx ? "ADD nginx.conf /etc/nginx/" : string.Empty)}

EXPOSE $PORT 

CMD {(enginx ? "service nginx start &&" : "")} {(customStartCommand.IsNullOrEmpty() ? "heroku local -p $PORT" : customStartCommand)}
";
        }
    }
}
