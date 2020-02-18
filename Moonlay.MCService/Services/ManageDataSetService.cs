using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moonlay.MasterData.Protos;
using Moonlay.MasterData.WebApi.Domain.DataSets;
using System.Threading.Tasks;
using System.Linq;

namespace Moonlay.MasterData.WebApi.Services
{
    public class ManageDataSetService : Protos.ManageDataSet.ManageDataSetBase
    {
        private readonly ILogger<ManageDataSetService> _logger;
        private readonly IDataSetService _dataSetService;

        public ManageDataSetService(ILogger<ManageDataSetService> logger, IDataSetService dataSetService)
        {
            _logger = logger;
            _dataSetService = dataSetService;
        }

        public override async Task<Reply> NewDataset(NewDatasetReq request, ServerCallContext context)
        {
            await _dataSetService.NewDataSet(request.Name, request.DomainName, request.OrganizationName, request.Attribute.Select(c => new Models.DataSetAttribute 
            { 
                DataSetName = request.Name,
                DomainName = request.DomainName,
                Name = c.Name,
                Type = c.Type,
                Value = c.Value,
                PrimaryKey = c.Primarykey,
                Null = c.Null
            }));


            return new Reply
            {
                Success = true,
                Message = "Successfully"
            };
        }

        public override async Task<Reply> RemoveDataSet(RemoveDataSetReq request, ServerCallContext context)
        {
            await _dataSetService.Remove(request.Name);

            return new Reply
            {
                Success = true,
                Message = "Successfully"
            };
        }
    }
}
