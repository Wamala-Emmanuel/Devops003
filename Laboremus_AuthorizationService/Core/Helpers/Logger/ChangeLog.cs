using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Laboremus_AuthorizationService.Core.Helpers.Logger
{
    public class ChangeLog
    {
        /// <summary>
        /// Mongo Db's primary key
        /// </summary>
        [BsonId]
        [JsonIgnore]
        public ObjectId _id { get; set; }
        /// <summary>
        /// Id - primary key
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Destination / Origin service e.g. Crm, Log, Ledger, Notification
        /// </summary>
        public ClientService Service { get; set; }

        /// <summary>
        /// Organisation Id
        /// </summary>
        public string OrganisationId { get; set; }

        /// <summary>
        /// Name of the entity e.g. Telephone, Contact, Organisation
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Date when the entry was originally created. Should be in UTC format
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// ID of the user that created the entry
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Action that needs to be taken : Add = 1, Update = 2, Delete = 3
        /// </summary>
        public Method Method { get; set; }

        /// <summary>
        /// Stores the date time when the entity is created at the server.
        /// </summary>
        public DateTime CreatedAtServer { get; set; }

        /// <summary>
        /// Data to be synchronized
        /// </summary>
        public object Data { get; set; }

        public ChangeLog()
        {
            CreatedAt = DateTime.UtcNow;
            CreatedAtServer = DateTime.UtcNow;
        }
    }
}
