#!/bin/sh
# This will update the repo and restart nginx

echo 'Pulling from repo'
git pull

echo 'Running Pipenv to get latest modules'
pipenv install

echo 'Restarting nginx and the daemon'
sudo systemctl restart nginx
sudo systemctl daemon-reload
sudo systemctl restart api_driver
