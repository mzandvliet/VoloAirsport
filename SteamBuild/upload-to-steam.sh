#!/bin/bash

if [ $# -eq 0 ]; then
  echo "Usage: ./upload-to-steam.sh password version_number (e.g. './upload-to-steam.sh Aap123 v3.4')"
  exit 0;
fi

# Get command-line args: password and version number
PASSWORD=$1
VERSION_NUMBER=$2

# Check if builds exist
if [ ! -d "../Build/win_32bit" ]; then
 echo "WARNING: no folder called 'win_32bit'"
fi
if [ ! -d "../Build/win_64bit" ]; then
 echo "WARNING: no folder called 'win_64bit'"
fi
if [ ! -d "../Build/mac" ]; then
 echo "WARNING: no folder called 'mac'"
fi
if [ ! -d "../Build/linux" ]; then
 echo "WARNING: no folder called 'linux'"
fi

#if [ -d "../Build/mac" ]; then
#    echo "Prepping MAC OS X content"
#    rm -r ../Build/mac_prepped
#    mkdir ../Build/mac_prepped
#    python ./ContentPrep.app/Contents/MacOS/contentprep.py --console --source=../Build/mac/volo_airsport.app --dest=../Build/mac_prepped/ --appid=329190 --noscramble
#fi
#
#if [ -d "../Build/linux" ]; then
#    echo "Prepping Linux content"
#    chmod +x ../Build/linux/run.sh
#    perl -pi -e 's/\r\n|\n|\r/\n/g' ../Build/linux/run.sh
#    chmod +x ../Build/linux/volo_airsport
#fi

# Replace version number in app.vdf file
cp ./scripts/app_build_329190.vdf.template ./scripts/app_build_329190.vdf
sed -i -e "s/@@version_number@@/$VERSION_NUMBER/g" ./scripts/app_build_329190.vdf
echo "Starting Steam Build process for Volo $VERSION_NUMBER ..."

# Run steam build command
./builder/steamcmd.exe +login ramjet_anvil_build $PASSWORD +run_app_build_http ../scripts/app_build_329190.vdf +quit
