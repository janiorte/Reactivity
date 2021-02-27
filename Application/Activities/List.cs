using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Activities
{
    public class List
    {
        public class Query : IRequest<List<ActivityDto>> { }

        public class Handler : IRequestHandler<Query, List<ActivityDto>>
        {
            private readonly DataContext Context;
            private readonly IMapper Mapper;

            public Handler(DataContext context, IMapper mapper)
            {
                Context = context;
                Mapper = mapper;
            }

            public async Task<List<ActivityDto>> Handle(Query request, CancellationToken cancellationToken)
            {
                return Mapper.Map<List<Activity>, List<ActivityDto>>(
                    await Context.Activities
                        .ToListAsync());
            }
        }
    }
}