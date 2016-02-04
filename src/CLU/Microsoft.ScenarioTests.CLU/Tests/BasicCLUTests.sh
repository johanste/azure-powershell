#!/bin/bash                                                                                                                                                                                                                                  set -e
set -e
DROPROOT=$1
RUNTIME=$2

printf "\nSetting up test package"
TESTDIR=$(dirname $0)
# to tar: tar -zcvf archive-name.tar.gz directory-name
tar -C ${DROPROOT}/pkgs/ -zxvf ${TESTDIR}/Microsoft.ScenarioTests.CLU.${RUNTIME}.tar.gz 
chmod 777 -R ${DROPROOT}/pkgs/Microsoft.ScenarioTests.CLU.${RUNTIME}
chmod +x -R ${DROPROOT}/pkgs/Microsoft.ScenarioTests.CLU.${RUNTIME}

printf "\n=== Basic CLU tests ===\n"

printf "\n1. Dispatch a command with no CLI name attribute"
${DROPROOT}/az progress show --Steps 5

printf "\n2. Dispatch a command with CLI name attribute"
${DROPROOT}/az returncode show

printf "\n3. Execute a command with no CLI name attribute directly"
${DROPROOT}/pkgs/Microsoft.ScenarioTests.CLU.${RUNTIME}/0.0.1/lib/dnxcore50/Microsoft.ScenarioTests.CLU progress show --Steps 5

printf "\n4. Execute a command with CLI name attribute directly"
${DROPROOT}/pkgs/Microsoft.ScenarioTests.CLU.${RUNTIME}/0.0.1/lib/dnxcore50/Microsoft.ScenarioTests.CLU returncode show

printf "\nCleaning up test package"
rm -R ${DROPROOT}/pkgs/Microsoft.ScenarioTests.CLU.${RUNTIME}

printf "\nSuccess!\n"
