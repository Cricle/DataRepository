using BenchmarkDotNet.Attributes;
using DataRepository.EFCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace DataRepository.Benchmark
{
    [MemoryDiagnoser]
    public class UpdateBuildBenchmark
    {
        [Benchmark]
        public void Raw()
        {
            Expression<Func<SetPropertyCalls<Student>, SetPropertyCalls<Student>>> x = y => y.SetProperty(q => q.A, 1);
        }

        [Benchmark]
        public void Builder()
        {
            new EFUpdateSetBuilder<Student>()
                .SetProperty(x => x.A, 1)
                .Build();
        }

        [Benchmark]
        public void RawExpression()
        {
            Expression<Func<SetPropertyCalls<Student>, SetPropertyCalls<Student>>> x = y => y.SetProperty(q => q.A, q => q.A + 1);
        }

        [Benchmark]
        public void BuilderExpression()
        {
            new EFUpdateSetBuilder<Student>()
                .SetProperty(x => x.A, x => x.A + 1)
                .Build();
        }
    }
    public class Student
    {
        public string Name { get; set; }

        public int A { get; set; }
    }
}
