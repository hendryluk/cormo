namespace Cormo.Injects
{
    public class ValueAttribute: QualifierAttribute
    {
        public string Name { get; private set; }
        public object Default { get; set; }

        public ValueAttribute()
        {
        }

        public ValueAttribute(string name)
        {
            Name = name;
        }
    }
}