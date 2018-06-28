#!/bin/sh

echo "Publish START"

app_name="CDHelper"
pub_dir=./bin/publish
current_dir=$PWD

rm -rf $pub_dir -f -r -v

dotnet publish --self-contained -c release -r linux-x64 -o $pub_dir/linux-x64
dotnet publish -c release -r win-x64 -o $pub_dir/win-x64
wait $(jobs -p)

cd $pub_dir/linux-x64/

zip -r ../$app_name-linux-x64.zip *

cd ../win-x64/

zip -r ../$app_name-win-x64.zip *

cd "$current_dir"

echo "Publish DONE"