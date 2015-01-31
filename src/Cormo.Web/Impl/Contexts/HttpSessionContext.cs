using System;
using System.Web;
using Cormo.Contexts;
using Cormo.Impl.Weld.Contexts;

namespace Cormo.Web.Impl.Contexts
{
    public class HttpSessionContext : AbstractManagedContext
    {
        private const string KEY_STORE = "cormo.componentstore";

        public override Type Scope
        {
            get { return typeof (SessionScopedAttribute); }
        }

        public override bool IsActive
        {
            get { return HttpContext.Current != null && HttpContext.Current.Session[KEY_STORE] != null; }
        }

        protected override IComponentStore ComponentStore
        {
            get { return (IComponentStore) HttpContext.Current.Items[KEY_STORE]; }
        }

        public override void Activate()
        {
            HttpContext.Current.Session.Add(KEY_STORE, new ConcurrentDictionaryComponentStore());
        }
        
        public override void Deactivate()
        {
            base.Deactivate();
            HttpContext.Current.Session.Remove(KEY_STORE);
        }

        
    }
}