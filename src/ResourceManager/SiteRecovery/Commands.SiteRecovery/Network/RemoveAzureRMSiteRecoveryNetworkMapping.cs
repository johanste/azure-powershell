﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.Azure.Management.SiteRecovery.Models;

namespace Microsoft.Azure.Commands.SiteRecovery
{
    /// <summary>
    /// Removes Azure Site Recovery Network mapping.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "AzureRmSiteRecoveryNetworkMapping")]
    [OutputType(typeof(ASRJob))]
    public class RemoveAzureRMSiteRecoveryNetworkMapping : SiteRecoveryCmdletBase
    {
        #region Parameters
        /// <summary>
        /// Gets or sets Network mapping object.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public ASRNetworkMapping NetworkMapping { get; set; }
        #endregion Parameters

        /// <summary>
        /// ProcessRecord of the command.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            try
            {
                LongRunningOperationResponse response = 
                    RecoveryServicesClient
                    .RemoveAzureSiteRecoveryNetworkMapping(
                    Utilities.GetValueFromArmId(this.NetworkMapping.ID, ARMResourceTypeConstants.ReplicationFabrics),
                    Utilities.GetValueFromArmId(this.NetworkMapping.ID, "replicationNetworks"),
                    Utilities.GetValueFromArmId(this.NetworkMapping.ID, "replicationNetworkMappings"));

                JobResponse jobResponse =
                    RecoveryServicesClient
                    .GetAzureSiteRecoveryJobDetails(PSRecoveryServicesClient.GetJobIdFromReponseLocation(response.Location));

                WriteObject(new ASRJob(jobResponse.Job));
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
            }
        }
    }
}