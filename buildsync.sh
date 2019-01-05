#!/bin/bash

cd ~/Nexplorer/
git pull
cd Nexplorer.Sync
sudo dotnet publish -c Release
sudo rm -rf /var/nexplorerSync
sudo cp -r ~/Nexplorer/Nexplorer.Sync/bin/Release/netcoreapp2.2/publish/ /var/nexplorerSync
sudo cp ~/.Nexplorer/connectionStrings.json /var/nexplorerSync/connectionStrings.json
sudo cp ~/.Nexplorer/emailConfig.json /var/nexplorerSync/emailConfig.json
sudo cp ~/.Nexplorer/userConfig.json /var/nexplorerSync/userConfig.json
sudo chgrp -R www-data /var/nexplorerSync
sudo chmod -R g+w /var/nexplorerSync
sudo supervisorctl restart nexplorerSync
echo "Nexplorer sync is running!"

