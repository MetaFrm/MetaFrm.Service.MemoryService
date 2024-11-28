using Microsoft.Extensions.Logging;
using System.Collections;

namespace MetaFrm.Service
{
    /// <summary>
    /// MemoryService 서비스를 구현합니다.
    /// </summary>
    public class MemoryService : IService
    {
        private readonly Hashtable Hashtable = [];

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

                response = this.Excute(serviceData);
            }
            catch (MetaFrmException exception)
            {
                Factory.Logger.LogError(exception, "{Message}", exception.Message);
                return new Response(exception);
            }
            catch (Exception exception)
            {
                Factory.Logger.LogError(exception, "{Message}", exception.Message);
                return new Response(exception);
            }

            return response;
        }

        Response Excute(ServiceData serviceData)
        {
            Response? response;

            response = new()
            {
                DataSet = new()
            };

            try
            {
                foreach (string table in serviceData.Commands.Keys)
                {
                    switch (serviceData.Commands[table].CommandText)
                    {
                        case "Get":
                            response.DataSet.DataTables.Add(new());
                            response.DataSet.DataTables[0].DataColumns.Add(new("VALUE", "System.String"));

                            for (int i = 0; i < serviceData[table].Values.Count; i++)
                            {
                                Data.DataRow dataRow1 = new();
                                response.DataSet.DataTables[0].DataRows.Add(dataRow1);

                                var key = serviceData[table].GetValue("KEY", i);
                                if (key != null)
                                    dataRow1.Values.Add("VALUE", new Data.DataValue(this.Hashtable[key]));
                                else
                                    dataRow1.Values.Add("VALUE", new Data.DataValue(null));
                            }
                            break;

                        case "Set":
                            for (int i = 0; i < serviceData[table].Values.Count; i++)
                            {
                                var key = serviceData[table].GetValue("KEY", i);
                                if (key != null)
                                {
                                    if (this.Hashtable.ContainsKey(key))
                                        this.Hashtable[key] = serviceData[table].GetValue("VALUE", i);
                                    else
                                        this.Hashtable.Add(key, serviceData[table].GetValue("VALUE", i));
                                }
                            }
                            break;
                    }
                }

                response.Status = Status.OK;
            }
            finally
            {
            }

            return response;
        }
    }
}