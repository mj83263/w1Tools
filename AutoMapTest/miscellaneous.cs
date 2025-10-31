using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapTest
{
    public class miscellaneous
    {
        public class Foo
        {
            public int ID { get; set; }

            public string Name { get; set; }
        }

        public class FooDto
        {
            public int ID { get; set; }

            public string Name { get; set; }
        }

        //public void Map()
        //{
        //    var config = new MapperConfiguration(cfg => cfg.CreateMap<Foo, FooDto>());

        //    var mapper = config.CreateMapper();

        //    Foo foo = new Foo { ID = 1, Name = "Tom" };

        //    FooDto dto = mapper.Map<FooDto>(foo);
        //}

    }
}
