using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contacts.API.Models;
using Contacts.API.Data;
using MongoDB.Driver;

namespace Contacts.API.Data
{
    public class MongoContactApplyRequestRepository : IContactApplyRequestRepository

    {
        private readonly ContactContext _contactContext;

        public MongoContactApplyRequestRepository(ContactContext contactContext)
        {
            _contactContext = contactContext;
        }

        public async Task<bool> AddRequestAsync(ContactApplyRequest reqeust,CancellationToken cancellationToken)
        {
            var filter =
                Builders<ContactApplyRequest>.Filter.Where(u =>
                    u.UserId == reqeust.UserId && u.ApplierId == reqeust.ApplierId);
            if ((await _contactContext.ContactApplyRequests.CountDocumentsAsync(filter)) > 0)
            {
                var update = Builders<ContactApplyRequest>.Update.Set(r => r.ApplyTime, DateTime.Now);
                var result =
                    await _contactContext.ContactApplyRequests.UpdateOneAsync(filter, update, null, cancellationToken);
                return result.MatchedCount == result.ModifiedCount && result.MatchedCount == 1;
            }

            await _contactContext.ContactApplyRequests.InsertOneAsync(reqeust, null, cancellationToken);
            return true;
        }
        
        public async Task<bool> ApprovalAsync(int userId,int applierId,CancellationToken cancellationToken)
        {
            var filter =
                Builders<ContactApplyRequest>.Filter.Where(r => r.UserId == userId && r.ApplierId == applierId);
            var update = Builders<ContactApplyRequest>.Update
                .Set(u => u.Approvaled, 1)
                .Set(u => u.HandleTime, DateTime.Now);
            var result =
                await _contactContext.ContactApplyRequests.UpdateOneAsync(filter, update, null, cancellationToken);
            return result.MatchedCount == result.ModifiedCount;
        }

        public async Task<List<ContactApplyRequest>> GetRequestListAsync(int userId,
            CancellationToken cancellationToken)
        {
            return (await _contactContext.ContactApplyRequests.FindAsync(u => u.UserId == userId,
                cancellationToken: cancellationToken)).ToList();
        }
    }
}