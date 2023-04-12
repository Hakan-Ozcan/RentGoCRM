using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class HGSFTPBL : BusinessHandler
    {
        public HGSFTPBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public HGSFTPBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public Guid CraeteOrUpdateFtpConsensus(int recordCount, string name)
        {
            Guid ftpConsensusId = Guid.Empty;
            QueryExpression getFtpConsensusQuery = new QueryExpression("rnt_ftpconsensus");
            getFtpConsensusQuery.ColumnSet = new ColumnSet("rnt_name");
            getFtpConsensusQuery.Criteria.AddCondition("rnt_name", ConditionOperator.Equal, name);
            EntityCollection getFtpConsensusList = this.OrgService.RetrieveMultiple(getFtpConsensusQuery);
            if (getFtpConsensusList.Entities.Count == 1)
            {
                ftpConsensusId = getFtpConsensusList.Entities[0].Id;
            }
            else
            {
                Entity ftpConsensus = new Entity("rnt_ftpconsensus");
                ftpConsensus.Attributes["rnt_recordcount"] = recordCount;
                ftpConsensus.Attributes["rnt_name"] = name;
                ftpConsensusId = this.OrgService.Create(ftpConsensus);
            }

            return ftpConsensusId;
        }

        public Guid CraeteFtpConsensusDetail(FtpConsensusDetail ftpConsensusDetail, Guid ftpConsensusId)
        {
            Entity ftpConsensus = new Entity("rnt_ftpconsensusdetail");
            ftpConsensus.Attributes["rnt_ftpconsensusid"] = new EntityReference("rnt_ftpconsensus", ftpConsensusId);
            ftpConsensus.Attributes["rnt_rowdetail"] = ftpConsensusDetail.rowDetail;
            ftpConsensus.Attributes["rnt_filerow"] = Convert.ToString(ftpConsensusDetail.fileRow);
            ftpConsensus.Attributes["rnt_exceptiondetail"] = ftpConsensusDetail.exceptionDetail;
            ftpConsensus.Attributes["rnt_transactionid"] = ftpConsensusDetail.transactionId;
            ftpConsensus.Attributes["rnt_amount"] = new Money(ftpConsensusDetail.amount);
            ftpConsensus.Id = this.OrgService.Create(ftpConsensus);
            return ftpConsensus.Id;
        }
    }
}
