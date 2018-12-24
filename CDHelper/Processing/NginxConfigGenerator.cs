using System;
using System.Collections.Generic;
using System.Text;

namespace CDHelper.Processing
{
    public static class NginxConfigGenerator
    {
        public static string GetSinglePortProxy(int sourcePort, int destinationPort, int worker_processes = 1, int worker_connections = 1024)
        {
            return $@"
worker_processes {worker_processes};
events {{ worker_connections {worker_connections}; }}
http {{
    server {{
    listen {sourcePort};
	server_name localhost;
    location / {{
			proxy_set_header X-Real-IP $remote_addr;
			proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
			proxy_set_header X-Forwarded-Proto $scheme;
			proxy_set_header Host $http_host;
			proxy_set_header X-NginX-Proxy true;
	
			proxy_pass http://127.0.0.1:{destinationPort};
			proxy_redirect off;
	
			# Socket.IO Support
			proxy_http_version 1.1;
			proxy_set_header Upgrade $http_upgrade;
			proxy_set_header Connection ""upgrade"";
        }}
    }}
}}";
        }
    }
}
