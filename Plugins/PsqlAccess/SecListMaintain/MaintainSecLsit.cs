using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsqlAccess.SecListMaintain;

public class MaintainSecLsit
{
    private readonly IDbContextFactory<AppDbContext> contextFactory;
    private readonly ILogger<MaintainSecLsit> logger;

    public MaintainSecLsit(IDbContextFactory<AppDbContext> contextFactory, ILogger<MaintainSecLsit> logger)
    {
        this.contextFactory = contextFactory;
        this.logger = logger;
    }
}