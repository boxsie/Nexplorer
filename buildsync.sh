#!/bin/bash

sudo supervisorctl stop nexplorerSync
cd ~/nexplorer/
git pull
cd Nexplorer.Sync
sudo dotnet publish -c Release
sudo cp ~/.nexplorer/connectionStrings.json ~/nexplorer/Nexplorer.Sync/bin/Release/netcoreapp2.1
sudo cp ~/.nexplorer/emailConfig.json ~/nexplorer/Nexplorer.Sync/bin/Release/netcoreapp2.1
sudo supervisorctl start nexplorerSync
echo "Nexplorer sync is running!"
