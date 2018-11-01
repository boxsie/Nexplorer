#!/bin/bash

cd ~/nexplorer/
git pull
cd Nexplorer.Sync
sudo dotnet publish -c Release
sudo cp ~/.nexplorer/connectionStrings.json ~/nexplorer/Nexplorer.Sync/bin/Release/netcoreapp2.1/publish/connectionStrings.json
sudo cp ~/.nexplorer/emailConfig.json ~/nexplorer/Nexplorer.Sync/bin/Release/netcoreapp2.1/publish/emailConfig.json
sudo cp ~/.nexplorer/userConfig.json ~/nexplorer/Nexplorer.Sync/bin/Release/netcoreapp2.1/publish/userConfig.json
sudo rm -rf /var/nexplorerSync
sudo cp -r ~/nexplorer/Nexplorer.Sync/bin/Release/netcoreapp2.1/publish/ /var/nexplorerSync/
sudo supervisorctl restart nexplorerSync
echo "Nexplorer sync is running!"
