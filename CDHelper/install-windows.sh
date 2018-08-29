#!/bin/sh

echo "Install START"
current_dir=$PWD

source ./publish.sh

zip_name="CDHelper-win-x64.zip"
install_dir="/d/CLI/CDHelper"

rm $install_dir/* -f -v
mv ./bin/publish/win-x64/* "$install_dir" -f -v

echo "Install DONE"

CDHelper -v