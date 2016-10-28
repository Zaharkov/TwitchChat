using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Models;

namespace Domain.Repositories
{
    public class AccessTokenRepository : BaseRepository<AccessToken>
    {
        private AccessTokenRepository()
        {
        }

        public static AccessTokenRepository Instance => new AccessTokenRepository();

        public void AddToken(AccessTokenType type, string value, int? expire = null)
        {
            var token = new AccessToken
            {
                Type = type,
                Value = value,
                Expire = expire.HasValue ? DateTime.UtcNow.AddSeconds(expire.Value) : (DateTime?) null
            };

            AddOrUpdate(token);
        }

        public string GetNotExpiredToken(AccessTokenType type, int? expireLag = null)
        {
            var time = DateTime.UtcNow.AddSeconds(expireLag ?? 0);
            var token =  Table.FirstOrDefault(t => t.Type == type && (!t.Expire.HasValue || (t.Expire.HasValue && t.Expire.Value > time)));

            return token?.Value;
        }

        public void DeleteToken(AccessTokenType type)
        {
            var token = GetById(type);

            if(token != null)
                Delete(token);
        }

        public List<AccessToken> GetTokens()
        {
            return Table.ToList();
        }
    }
}
