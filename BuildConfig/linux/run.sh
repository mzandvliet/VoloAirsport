#!/bin/bash

# Move to script's directory
cd "`dirname "$0"`"

# Get the kernel/architecture information
ARCH=`uname -m`

# Set the libpath and pick the proper binary
if [ "$ARCH" == "x86_64" ]; then
    ./volo_airsport.x86_64 $@
else
    ./volo_airsport.x86 $@
fi
