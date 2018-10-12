#!/bin/bash

sudo supervisorctl stop nexplorerSync
cd ~/Nexplorer/
git pull
cd Nexplorer.Sync
sudo dotnet publish -c Release
sudo supervisorctl start nexplorerSync
echo "Nexplorer sync is running!"
