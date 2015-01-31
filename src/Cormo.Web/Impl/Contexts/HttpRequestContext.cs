using System;
using System.Web;
using Cormo.Contexts;
using Cormo.Impl.Weld.Contexts;

namespace Cormo.Web.Impl.Contexts
{
    public class HttpRequestContext : AbstractManagedContext
    {
        private const string KEY_STORE = "cormo.componentstore";

        public override Type Scope
        {
            get { return typeof (RequestScopedAttribute); }
        }

        public override bool IsActive
        {
            get { return HttpContext.Current != null && HttpContext.Current.Items.Contains(KEY_STORE); }
        }

        protected override IComponentStore ComponentStore
        {
            get { return (IComponentStore) HttpContext.Current.Items[KEY_STORE]; }
        }

        public override void Activate()
        {
            HttpContext.Current.Items[KEY_STORE] = new ConcurrentDictionaryComponentStore();
        }
        
        public override void Deactivate()
        {
            base.Deactivate();
            HttpContext.Current.Items.Remove(KEY_STORE);
        }

        
    }
}