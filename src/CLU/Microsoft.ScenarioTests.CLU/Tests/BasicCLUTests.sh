#!/bin/bash
set -e
DROPROOT=$1
RUNTIME=$2
STORAGECONTAINER=https://clutest.blob.core.windows.net/testpackages

usage () {
	printf "\nUsage: <script> <DropRoot> <Runtime>\nExample: $0 ~/clurun/osx.10.10-x64 osx.10.10-x64"
	exit -1
}

[ "$#" -eq 2 ] || usage

printf "\nSetting up test package"
TESTDIR=$(dirname $0)
cd $TESTDIR
curl ${STORAGECONTAINER}/Microsoft.ScenarioTests.CLU.${RUNTIME}.tar.gz > ${TESTDIR}/Microsoft.ScenarioTests.CLU.${RUNTIME}.tar.gz
# To rebuild tar file: add Microsoft.ScenarioTests.CLU to BuildAndInstallCLU.bat,
# tar -zcvf archive-name.tar.gz directory-name
tar -C ${DROPROOT}/pkgs/ -zxvf ${TESTDIR}/Microsoft.ScenarioTests.CLU.${RUNTIME}.tar.gz 
chmod -R +x ${DROPROOT}/pkgs/Microsoft.ScenarioTests.CLU.${RUNTIME}
chmod -R 777 ${DROPROOT}/pkgs/Microsoft.ScenarioTests.CLU.${RUNTIME}

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
rm ${TESTDIR}/Microsoft.ScenarioTests.CLU.${RUNTIME}.tar.gz

printf "\nSuccess!\n"
