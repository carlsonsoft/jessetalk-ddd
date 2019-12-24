using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contacts.API.Dtos;
using Contacts.API.Models;
using MongoDB.Driver;


namespace Contacts.API.Data
{
    public class MongoContactRepository : IContactRepository
    {
        private readonly ContactContext _contactContext;

        public MongoContactRepository(ContactContext contactContext)
        {
            _contactContext = contactContext;
        }

        public async Task<bool> UpdateContactInfoAsync(BaseUserInfo userInfo, CancellationToken cancellationToken)
        {
            var contactBook =
                (await _contactContext.ContactBooks.FindAsync(u => u.UserId == userInfo.UserId, null, cancellationToken)
                ).FirstOrDefault();
            if (contactBook == null)
            {
                return true;
            }

            var contactIds = contactBook.Contacts.Select(u => u.UserId);
            var filter = Builders<ContactBook>.Filter.And(Builders<ContactBook>.Filter.In(u => u.UserId, contactIds),
                Builders<ContactBook>.Filter.ElemMatch(u => u.Contacts, contact => contact.UserId == userInfo.UserId));
            var update = Builders<ContactBook>.Update.Set("Contact.$.Name", userInfo.Name)
                .Set("Contact.$.Avatar", userInfo.Avatar)
                .Set("Contact.$.Company", userInfo.Company)
                .Set("Contact.$.Title", userInfo.Title);
            var updateResult = _contactContext.ContactBooks.UpdateMany(filter, update);
            return updateResult.MatchedCount == updateResult.ModifiedCount;
        }

        public async Task<bool> AddContactAsync(int userId, BaseUserInfo contact, CancellationToken cancellationToken)
        {
            if ((await _contactContext.ContactBooks.CountDocumentsAsync(u => u.UserId == userId)) > 0)
            {
                await _contactContext.ContactBooks.InsertOneAsync(new ContactBook() {UserId = userId});
            }

            var filter = Builders<ContactBook>.Filter.Eq(c => c.UserId, userId);
            var update = Builders<ContactBook>.Update.AddToSet(c => c.Contacts, new Models.Contact()
            {
                UserId = contact.UserId,
                Avatar = contact.Avatar,
                Company = contact.Company,
                Name = contact.Name,
                Title = contact.Title
            });
            var result = await _contactContext.ContactBooks.UpdateOneAsync(filter, update, null, cancellationToken);
            return result.ModifiedCount == result.MatchedCount;
        }
    }
}