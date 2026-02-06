using System.Collections.Concurrent;

namespace MetaFrm.Service
{
    /// <summary>
    /// MemoryService 서비스를 구현합니다.
    /// </summary>
    public class MemoryService : IService
    {
        private readonly ConcurrentDictionary<string, string?> _memory = [];

        /// <summary>
        /// 생성자 입니다.
        /// MemoryService를 생성합니다.
        /// </summary>
        public MemoryService()
        {
        }

        Response IService.Request(ServiceData serviceData)
        {
            Response response;

            try
            {
                if (serviceData.ServiceName == null || !serviceData.ServiceName.Equals("MetaFrm.Service.MemoryService"))
                    throw new Exception("Not MetaFrm.Service.MemoryService");

                response = this.Execute(serviceData);
            }
            catch (Exception exception)
            {
                Factory.Logger.Error(exception, "{0}", exception.Message);
                return new Response(exception);
            }

            return response;
        }

        private Response Execute(ServiceData serviceData)
        {
            Response response;

            response = new();

            foreach (string cmdKey in serviceData.Commands.Keys)
            {
                switch (serviceData.Commands[cmdKey].CommandText)
                {
                    case "Get":
                        var table = new Data.DataTable();
                        table.DataColumns.Add(new("VALUE", typeof(string).FullName!));

                        response.DataSet ??= new();
                        response.DataSet.DataTables.Add(table);

                        for (int i = 0; i < serviceData[cmdKey].Values.Count; i++)
                        {
                            Data.DataRow dataRow = new();

                            var key = serviceData[cmdKey].GetValue("KEY", i);
                            if (key != null && key is string str)
                            {
                                this._memory.TryGetValue(str, out var value1);
                                dataRow.Values.Add("VALUE", new Data.DataValue(value1));
                            }
                            else
                                dataRow.Values.Add("VALUE", new Data.DataValue(null));

                            table.DataRows.Add(dataRow);
                        }
                        break;

                    case "Set":
                        for (int i = 0; i < serviceData[cmdKey].Values.Count; i++)
                        {
                            var key = serviceData[cmdKey].GetValue("KEY", i);
                            if (key != null && key is string str)
                            {
                                var value = (string?)serviceData[cmdKey].GetValue("VALUE", i);
                                _memory.AddOrUpdate(str, value, (_, _) => value);
                            }
                        }
                        break;
                }
            }

            response.Status = Status.OK;

            return response;
        }

        Task<Response> IService.RequestAsync(ServiceData serviceData)
        {
            Response response = ((IService)this).Request(serviceData);

            return Task.FromResult(response);
        }
    }
}