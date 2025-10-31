using System.Diagnostics.CodeAnalysis;

namespace autoMapExLib
{

    public class Tc1
    {
        public string Name { set; get; }
        public Int32 Id { set; get; }

    }
   public class Tc2
    {
        public string Name { set; get; }
        public Int32 Id { set; get; }
    }
    public class MyProfile:AutoMapper.Profile
    {
        public MyProfile()
        {
            //CreateProjection<Tc1, Tc2>();
            CreateMap<Tc1, Tc2>();
        }
    }
}
