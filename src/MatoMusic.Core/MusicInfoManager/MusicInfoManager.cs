using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.EntityFrameworkCore.Repositories;
using Android.Database;
using Android.Provider;
using MatoMusic.Core.Interfaces;
using MatoMusic.Core.Models;
using MatoMusic.Core.ViewModel;
using MatoMusic.Infrastructure;
using MatoMusic.Infrastructure.Helper;
using Microsoft.International.Converters.PinYinConverter;

namespace MatoMusic.Core
{
    public partial class MusicInfoManager :ISingletonDependency
    {
       
        

    }
}
