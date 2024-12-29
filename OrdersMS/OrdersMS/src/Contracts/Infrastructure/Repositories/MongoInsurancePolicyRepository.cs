using MongoDB.Bson;
using MongoDB.Driver;
using OrdersMS.Core.Infrastructure.Data;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Queries.GetAllPolicies.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Contracts.Infrastructure.Models;

namespace OrdersMS.src.Contracts.Infrastructure.Repositories
{
    public class MongoInsurancePolicyRepository(MongoDbService mongoDbService) : IPolicyRepository
    {
        private readonly IMongoCollection<BsonDocument> _policyCollection = mongoDbService.GetInsurancePolicyCollection();

        public async Task<bool> ExistByType(string type)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("type", type);
            var policy = await _policyCollection.Find(filter).FirstOrDefaultAsync();
            return policy != null;
        }

        public async Task<List<InsurancePolicy>> GetAll(GetAllPoliciesQuery data)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = data.IsActive?.ToLower() switch
            {
                "active" => filterBuilder.Eq("isActive", true),
                "inactive" => filterBuilder.Eq("isActive", false),
                _ => filterBuilder.Empty
            };

            var policyEntities = await _policyCollection
                .Find(filter)
                .Skip(data.PerPage * (data.Page - 1))
                .Limit(data.PerPage)
                .ToListAsync();

            var policies = policyEntities.Select(p =>
            {
                var policy = new InsurancePolicy
                (
                    new PolicyId(p.GetValue("_id").AsString),
                    new PolicyType(p.GetValue("policyType").AsString),
                    new PolicyCoverageKm(p.GetValue("coverageKm").AsInt32),
                    new PolicyIncidentCoverageAmount(Math.Round(p.GetValue("coverageAmount").AsDouble, 2)),
                    new PriceExtraKm(Math.Round(p.GetValue("priceExtraKm").AsDouble, 2))
                );

                policy.SetIsActive(p.GetValue("isActive").AsBoolean);
                return policy;
            }).ToList();

            return policies;
        }

        public async Task<Core.Utils.Optional.Optional<InsurancePolicy>> GetById(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var policyDocument = await _policyCollection.Find(filter).FirstOrDefaultAsync();

            if (policyDocument is null)
            {
                return Core.Utils.Optional.Optional<InsurancePolicy>.Empty();
            }

            var policy = new InsurancePolicy(
                new PolicyId(policyDocument.GetValue("_id").AsString),
                new PolicyType(policyDocument.GetValue("policyType").AsString),
                new PolicyCoverageKm(policyDocument.GetValue("coverageKm").AsInt32),
                new PolicyIncidentCoverageAmount(Math.Round(policyDocument.GetValue("coverageAmount").AsDouble, 2)),
                new PriceExtraKm(Math.Round(policyDocument.GetValue("priceExtraKm").AsDouble, 2))
            );

            policy.SetIsActive(policyDocument.GetValue("isActive").AsBoolean);

            return Core.Utils.Optional.Optional<InsurancePolicy>.Of(policy);
        }

        public async Task<Result<InsurancePolicy>> Save(InsurancePolicy policy)
        {
            var mongoPolicy = new MongoInsurancePolicy
            {
                Id = policy.GetId(),
                PolicyType = policy.GetPolicyType(),
                CoverageKm = policy.GetPolicyCoverageKm(),
                CoverageAmount = Math.Round(policy.GetPolicyIncidentCoverageAmount(), 2),
                PriceExtraKm = Math.Round(policy.GetPriceExtraKm(), 2),
                IsActive = policy.GetIsActive()
            };

            var bsonDocument = new BsonDocument
            {
                {"_id", mongoPolicy.Id},
                {"policyType", mongoPolicy.PolicyType},
                {"coverageKm", mongoPolicy.CoverageKm},
                {"coverageAmount", mongoPolicy.CoverageAmount},
                {"priceExtraKm", mongoPolicy.PriceExtraKm},
                {"isActive", mongoPolicy.IsActive}
            };

            await _policyCollection.InsertOneAsync(bsonDocument);

            var savedPolicy = new InsurancePolicy(
                new PolicyId(mongoPolicy.Id),
                new PolicyType(mongoPolicy.PolicyType),
                new PolicyCoverageKm(mongoPolicy.CoverageKm),
                new PolicyIncidentCoverageAmount(mongoPolicy.CoverageAmount),
                new PriceExtraKm(mongoPolicy.PriceExtraKm)
            );

            return Result<InsurancePolicy>.Success(savedPolicy);
        }

        public async Task<Result<InsurancePolicy>> Update(InsurancePolicy policy)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", policy.GetId());
            var update = Builders<BsonDocument>.Update
                .Set("isActive", policy.GetIsActive());

            var updateResult = await _policyCollection.UpdateOneAsync(filter, update);

            if (updateResult.ModifiedCount == 0)
            {
                return Result<InsurancePolicy>.Failure(new Exception("Failed to update policy"));
            }

            return Result<InsurancePolicy>.Success(policy);
        }

        public async Task<bool> IsActivePolicy(string id)
        {
            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq("_id", id),
                Builders<BsonDocument>.Filter.Eq("isActive", true)
            );

            var policy = await _policyCollection.Find(filter).FirstOrDefaultAsync();
            return policy != null;
        }
    }
}
