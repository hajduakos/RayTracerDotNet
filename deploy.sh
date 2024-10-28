#!/bin/bash

set -e # exit with nonzero exit code if anything fails

# clear and re-create the out directory
rm -rf out || exit 0
mkdir out

# copy all bmps
cp -r Scenes/*.bmp out/ || true

cd out
echo "<html><head></head><body style=\"color: white; background-color: #333; text-align: center;\">" > index.html
for f in *.bmp
do
    echo "$f<br />" >> index.html
    echo "<img src=\"$f\" /> <br/><br />" >> index.html
done
echo "</body></html>" >> index.html
