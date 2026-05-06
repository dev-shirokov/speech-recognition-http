namespace api.Infrastructure.Persist;

public class EntityNotFoundException : ArgumentNullException
{
    public EntityNotFoundException(string paramName) 
        : base(paramName, $"Entity '{paramName}' in persist storeage not found")
    {

    }
}