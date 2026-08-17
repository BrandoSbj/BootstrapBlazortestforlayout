using BootstrapBlazor.Components;
using BootstrapBlazortestforlayout.Server.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace BootstrapBlazortestforlayout.Server.Components.Pages
{
    public partial class TestTable : ComponentBase
    {
        [Inject]
        [NotNull]
        private IStringLocalizer<Foo>? Localizer { get; set; }

        private readonly ConcurrentDictionary<Foo, IEnumerable<SelectedItem>> _cache = new();

        private IEnumerable<SelectedItem> GetHobbies(Foo item) => _cache.GetOrAdd(item, f => Foo.GenerateHobbies(Localizer));

        private static IEnumerable<int> PageItemsSource => new int[] { 20, 40 };

        [NotNull]
        private List<Foo>? Items { get; set; }

        private bool modalVisible;
        [NotNull]
        private Modal? Modal1 = new();
        private string DebugInfo { get; set; } = "";
        private async Task Modal1OnShow()
        {
            await Modal1.Show();
        }
        //private async Task Modal1OnShow()
        //{
        //    if (Modal1 is null)
        //    {
        //        DebugInfo = "Modal1 is null when attempting to show";
        //        await InvokeAsync(StateHasChanged);
        //        return;
        //    }

        //    var type = Modal1.GetType();
        //    var mi = type.GetMethod("ShowAsync");
        //    if (mi != null)
        //    {
        //        var task = (System.Threading.Tasks.Task)mi.Invoke(Modal1, null)!;
        //        await task;
        //        DebugInfo = "Called ShowAsync on Modal1";
        //        await InvokeAsync(StateHasChanged);
        //        return;
        //    }

        //    mi = type.GetMethod("Show");
        //    if (mi != null)
        //    {
        //        var result = mi.Invoke(Modal1, null);
        //        if (result is System.Threading.Tasks.Task t)
        //        {
        //            await t;
        //            DebugInfo = "Called Show (task) on Modal1";
        //        }
        //        else
        //        {
        //            DebugInfo = "Called Show on Modal1";
        //        }
        //        await InvokeAsync(StateHasChanged);
        //        return;
        //    }

        //    modalVisible = true;
        //    DebugInfo = "Fallback set modalVisible = true";
        //    await InvokeAsync(StateHasChanged);
        //}

        private async Task Modal1OnHide()
        {
            if (Modal1 is null)
            {
                modalVisible = false;
                DebugInfo = "Modal1 is null when attempting to hide, fallback set modalVisible = false";
                await InvokeAsync(StateHasChanged);
                return;
            }

            var type = Modal1.GetType();
            var mi = type.GetMethod("HideAsync");
            if (mi != null)
            {
                var task = (System.Threading.Tasks.Task)mi.Invoke(Modal1, null)!;
                await task;
                DebugInfo = "Called HideAsync on Modal1";
                await InvokeAsync(StateHasChanged);
                return;
            }

            mi = type.GetMethod("Hide");
            if (mi != null)
            {
                var result = mi.Invoke(Modal1, null);
                if (result is System.Threading.Tasks.Task t)
                {
                    await t;
                    DebugInfo = "Called Hide (task) on Modal1";
                }
                else
                {
                    DebugInfo = "Called Hide on Modal1";
                }
                await InvokeAsync(StateHasChanged);
                return;
            }

            modalVisible = false;
            DebugInfo = "Fallback set modalVisible = false";
            await InvokeAsync(StateHasChanged);
        }

        // Removed legacy OpenModal/CloseModal that referenced non-existent tableModal

        private Task<QueryData<Foo>> OnQueryAsync(QueryPageOptions options)
        {
            if (Items == null || Items.Count == 0)
            {
                Items = Foo.GenerateFoo(Localizer);
            }

            var items = Items.Where(options.ToFilterFunc<Foo>());
            var isSorted = false;
            if (!string.IsNullOrEmpty(options.SortName))
            {
                items = items.Sort(options.SortName, options.SortOrder);
                isSorted = true;
            }

            var total = items.Count();
            return Task.FromResult(new QueryData<Foo>()
            {
                Items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList(),
                TotalCount = total,
                IsFiltered = true,
                IsSorted = isSorted,
                IsSearch = true
            });
        }

        private Task<bool> OnSaveAsync(Foo foo, ItemChangedType changedType)
        {
            var ret = false;
            if (changedType == ItemChangedType.Add)
            {
                var id = Items.Count + 1;
                while (Items.Find(item => item.Id == id) != null)
                {
                    id++;
                }
                var item = new Foo()
                {
                    Id = id,
                    Name = foo.Name,
                    Address = foo.Address,
                    Complete = foo.Complete,
                    Count = foo.Count,
                    DateTime = foo.DateTime,
                    Education = foo.Education,
                    Hobby = foo.Hobby
                };
                Items.Add(item);
            }
            else
            {
                var f = Items.Find(i => i.Id == foo.Id);
                if (f != null)
                {
                    f.Name = foo.Name;
                    f.Address = foo.Address;
                    f.Complete = foo.Complete;
                    f.Count = foo.Count;
                    f.DateTime = foo.DateTime;
                    f.Education = foo.Education;
                    f.Hobby = foo.Hobby;
                }
            }
            ret = true;
            return Task.FromResult(ret);
        }

        private Task<bool> OnDeleteAsync(IEnumerable<Foo> foos)
        {
            foreach (var foo in foos)
            {
                Items.Remove(foo);
            }

            return Task.FromResult(true);
        }

        // Keep existing CloseModal usage by aliasing to Modal1OnHide
        private Task CloseModal() => Modal1OnHide();
    }
}
