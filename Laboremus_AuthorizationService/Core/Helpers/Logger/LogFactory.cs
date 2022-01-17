using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Laboremus_AuthorizationService.Core.Helpers.Logger
{
    public static class Logger
    {
        public static async Task DoAccessLogAsync(string url, AccessLog log)
        {
            var logs = new List<Log>
            {
                new Log
                {
                    Type = LogType.Access,
                    MetaData = log
                }
            };

            var json = JsonConvert.SerializeObject(logs, Formatting.Indented);
            var buffer = Encoding.UTF8.GetBytes(json);
            var content = new ByteArrayContent(buffer);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (var client = new HttpClient())
            {
                await client.PostAsync($"{url}/api/log/add", content);
            }
        }

        public static async Task DoExceptionLogAsync(string url, Exception exception, ClientService service, string userId, string organisationId)
        {
            var customException = new ExceptionLog
            {
                Date = DateTime.UtcNow,
                Service = service.ToString(),
                UserId = userId,
                IpAddress = null,
                OrganisationId = organisationId,
                Exception = new CustomException
                {
                    Message = exception.Message,
                    Type = exception.GetType().Name,
                    StackTrace = exception.StackTrace,
                    InnerException = new InnerException
                    {
                        Message = exception.InnerException?.Message,
                        Type = exception.InnerException?.GetType()?.Name,
                        StackTrace = exception.InnerException?.StackTrace
                    }
                }
            };

            var logs = new List<Log>
            {
                new Log
                {
                    Type = LogType.Exception,
                    MetaData = customException
                }
            };

            var json = JsonConvert.SerializeObject(logs, Formatting.Indented);
            var buffer = Encoding.UTF8.GetBytes(json);
            var content = new ByteArrayContent(buffer);

            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (var client = new HttpClient())
            {
                await client.PostAsync($"{url}/api/log/add", content);
            }
        }

        public static async Task DoChangeLogAsync(string url, List<EntityEntry> entities, ClientService service, string userId, string organisationId)
        {
            if (entities != null && entities.Any())
            {
                var logs = new List<Log>();

                foreach (var entity in entities)
                {
                    var pk = entity.Metadata.FindPrimaryKey().Properties.Select(x => x.Name).Single();
                    var id = entity.CurrentValues[pk];

                    var log = new ChangeLog
                    {
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId,
                        OrganisationId = organisationId,
                        Service = service,
                        EntityName = entity.Metadata.Name?.Split(".").Reverse().First(),
                        Id = Guid.Parse(id.ToString())
                    };

                    var type = LogType.Add;
                    switch (entity.State)
                    {
                        case EntityState.Detached:
                            break;
                        case EntityState.Unchanged:
                            break;
                        case EntityState.Deleted:
                            type = LogType.Delete;
                            log.Method = Method.Delete;
                            SetDeletedProperties(entity, log);
                            break;
                        case EntityState.Modified:
                            type = LogType.Modify;
                            log.Method = Method.Update;
                            SetModifiedProperties(entity, log);
                            break;
                        case EntityState.Added:
                            type = LogType.Add;
                            log.Method = Method.Add;
                            SetAddedProperties(entity, log);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    logs.Add(new Log
                    {
                        Type = type,
                        MetaData = log
                    });

                    var json = JsonConvert.SerializeObject(logs, Formatting.Indented);
                    var buffer = Encoding.UTF8.GetBytes(json);
                    var content = new ByteArrayContent(buffer);

                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    using (var client = new HttpClient())
                    {
                        try
                        {
                            await client.PostAsync($"{url}/api/log/add", content);
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.Message);
                        }

                    }
                }
            }
        }

        #region Private Methods
        private static void SetAddedProperties(EntityEntry entry, ChangeLog log)
        {
            try
            {
                var jObject = new JObject();

                foreach (var propertyName in entry.CurrentValues.Properties)
                {
                    var newVal = entry.CurrentValues[propertyName];
                    if (newVal != null)
                    {
                        var name = propertyName.Name.ToLowerCamelCase();
                        jObject.Add(new JProperty(name, newVal));
                    }
                }

                log.Data = jObject;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private static void SetDeletedProperties(EntityEntry entry, ChangeLog log)
        {
            try
            {
                var dbValues = entry.GetDatabaseValues();

                var jObject = new JObject();
                foreach (var property in dbValues.Properties)
                {

                    var oldVal = dbValues[property];
                    if (oldVal != null)
                    {
                        var name = property.Name.ToLowerCamelCase();
                        jObject.Add(new JProperty(name, oldVal));
                    }
                }
                log.Data = jObject;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        private static void SetModifiedProperties(EntityEntry entry, ChangeLog log)
        {
            try
            {
                var dbValues = entry.GetDatabaseValues();

                var jObject = new JObject();

                foreach (var property in entry.OriginalValues.Properties)
                {
                    var oldVal = dbValues[property];
                    var newVal = entry.CurrentValues[property];

                    if (oldVal != null && newVal != null && !Equals(oldVal, newVal))
                    {
                        var name = property.Name.ToLowerCamelCase();
                        jObject.Add(new JProperty(name, newVal));
                    }
                }

                log.Data = jObject;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }
        #endregion

    }
}
