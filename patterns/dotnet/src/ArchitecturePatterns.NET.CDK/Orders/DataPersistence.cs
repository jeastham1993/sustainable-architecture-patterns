using Amazon.CDK.AWS.DynamoDB;
using Constructs;

namespace ArchitecturePatterns.NET.CDK.Orders;

public record DataPersistenceProps(string Name);

public class DataPersistence : Construct
{
    public ITable Table { get; private set; }
    
    public DataPersistence(Construct scope, string id, DataPersistenceProps props) : base(scope, id)
    {
        this.Table = new Table(this, $"{props.Name}Table", new TableProps
        {
            TableName = props.Name,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            PartitionKey = new Attribute()
            {
                Name = "PK",
                Type = AttributeType.STRING
            },
            SortKey = new Attribute()
            {
                Name = "SK",
                Type = AttributeType.STRING
            },
            Stream = StreamViewType.NEW_AND_OLD_IMAGES
        });
    }
}