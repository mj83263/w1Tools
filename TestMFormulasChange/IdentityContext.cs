using KFCK.ThicknessMeter.Communication;
using System.Runtime.CompilerServices;

namespace KFCK.Entities
{
    //
    // 摘要:
    //     主体身份上下文对象，用于表示当前操作人的身份
    public class IdentityContext
    {
        public bool IsRemote { get; set; }

        public bool HasPrincipal { get; set; }
        
        public string FrontendDisplayName { set; get; } 

        public static readonly IdentityContext SystemIdentity = new IdentityContext("系统");
        public IdentityContext(string FrontendDisplayName)
        {
            this.FrontendDisplayName = FrontendDisplayName;
        }

        [CompilerGenerated]
        protected IdentityContext(IdentityContext original)
        {
            FrontendDisplayName = original.FrontendDisplayName;
            IsRemote = original.IsRemote;
            HasPrincipal = original.HasPrincipal;
        }
    }

}



