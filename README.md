# CDHelper
Continuous Deployment CLI Helper Tool

#To Install or update CDHelper on linux:

grab latest version from: https://github.com/asmodat/CDHelper/releases

cd /usr/local/src

rm -fv ./CDHelper-linux-x64.zip

wget https://github.com/asmodat/CDHelper/releases/download/0.0.4/CDHelper-linux-x64.zip

rm -rfv  /usr/local/bin/CDHelper

unzip CDHelper-linux-x64.zip -d /usr/local/bin/CDHelper

chmod -R 777 /usr/local/bin/CDHelper

nano /etc/profile.d/execute.sh

#save at the end of the file then click Ctrl+O, [enter] Ctrl+X

export PATH=/usr/local/bin/CDHelper:$PATH
