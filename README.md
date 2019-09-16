# CDHelper
Continuous Deployment CLI Helper Tool

## Installation

> Simply, fast and dirty method, do NOT use in production

```
sudo -s

CDHelperVersion="v0.5.0" && \
 cd /usr/local/src && \
 rm -f -v ./CDHelper-linux-x64.zip && \
 wget https://github.com/asmodat/CDHelper/releases/download/$CDHelperVersion/CDHelper-linux-x64.zip && \
 rm -rfv /usr/local/bin/CDHelper && \
 unzip CDHelper-linux-x64.zip -d /usr/local/bin/CDHelper && \
 chmod -R 777 /usr/local/bin/CDHelper
```
> Create new service by appending following script using `nano /lib/systemd/system/cosmos.service` command.

```
[Unit]
Description=Asmodat Deployment Scheduler
After=network.target

[Service]
Type=simple
ExecStart=/usr/local/bin/CDHelper/CDHelper scheduler github
WorkingDirectory=/home/ubuntu
Restart=always
RestartSec=5

[Install]
WantedBy=default.target

```
> Enable service, start and check logs

```
systemctl enable scheduler && \
 systemctl restart scheduler && \
 journalctl --unit=scheduler -n 100 --no-pager
```



