#!/bin/bash

cd ~/Nexplorer/
git pull
cd Nexplorer.Sync
sudo dotnet publish -c Release
sudo cp ~/.Nexplorer/connectionStrings.json ~/Nexplorer/Nexplorer.Sync/bin/Release/netcoreapp2.2/publish/connectionStrings.json
sudo cp ~/.Nexplorer/emailConfig.json ~/Nexplorer/Nexplorer.Sync/bin/Release/netcoreapp2.2/publish/emailConfig.json
sudo cp ~/.Nexplorer/userConfig.json ~/Nexplorer/Nexplorer.Sync/bin/Release/netcoreapp2.2/publish/userConfig.json
sudo rm -rf /var/nexplorerSync
sudo cp -r ~/Nexplorer/Nexplorer.Sync/bin/Release/netcoreapp2.2/publish/ /var/nexplorerSync
sudo supervisorctl restart nexplorerSync
echo "Nexplorer sync is running!"
