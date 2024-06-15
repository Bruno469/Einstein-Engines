#!/bin/bash

COMMITS=(
  "4560afe3a5976f043c678d5aa88d419459e36dff"
  "a713a1451f33601aea5a016789891ede39784c63"
  "e3f812b104b6f0159a4e465ba82838e263ea227e"
  "08862b4a740d8cdb3715d3135b4a19209cf76ffc"
  "98ac5b32da9be58ab90778bb2704c2330d18a649"
  "68e1355711107f84b4c1d4f967646d0832024a93"
  "01d84a95f8c29cfc1ba547960462d7002b6a73f2"
  "b08e94035ee3d28e8f5c7766acc1c2a4c76310fa"
  "96fd320726b0a76638712922d1a89ffbd2cde877"
  "404d47c4f6dc7d214532cfc16787089873db76de"
  "f963a55fc1317a1e4251db52289260071a1e2992"
  "74701fdb9fbb64ca96e6b4ac3175d3c19a0fb47a"
)

for COMMIT in "${COMMITS[@]}"
do
  git cherry-pick $COMMIT
  if [ $? -ne 0 ]; then
    git checkout --theirs .
    git add .
    git cherry-pick --continue
  fi
done
