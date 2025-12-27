using Lira.Common;
using Lira.Common.Exceptions;
using Lira.Domain.Configuration;
using Lira.Domain.DataModel;
using Microsoft.AspNetCore.Mvc;

namespace Lira.Controllers;

[ApiController]
public class SysController : ControllerBase
{
    [HttpGet("/")]
    public async Task<ActionResult> State([FromServices]IConfigurationLoader loader)
    {
        var state = await loader.GetState();

        if (state is ConfigurationState.Ok ok)
        {
            return Ok($"Ok. Load time: {ok.LoadTime} " +
                      $"({(int)(DateTime.Now - ok.LoadTime).TotalSeconds} second ago)");
        }

        if(state is ConfigurationState.Error error)
        {
            return StatusCode(500, $"Error. Load time: {error.LoadTime} " +
                                              $"({(int)(DateTime.Now - error.LoadTime).TotalSeconds} second ago)" +
                                              Constants.NewLine + Constants.NewLine +
                                              error.Exception);
        }

        throw new UnsupportedInstanceType(state);
    }

    [HttpGet("/range/val/{name}/{rangeName}/{count}")]
    public ActionResult GetRangeValue(string name, int? count, string rangeName, IRangesProvider rangesProvider)
    {
        var data = rangesProvider.Find(new DataName(name));

        if (data == null)
            return NotFound($"Range '{name}' not found");

        var range = data.Find(new DataName(rangeName));

        if (range == null)
        {
            return NotFound($"Range '{rangeName}' in interval '{name}' not found");
        }

        return Ok(string.Join(
            Constants.NewLine,
            Enumerable.Repeat(1,  count ?? 20)
                .Select(_ => range.NextValue())));
    }

    [HttpGet("/range/info")]
    public string GetRangesInfo(IRangesProvider rangesProvider) => GetRangesText(rangesProvider.GetAll());

    [HttpGet("/range/info/{name}")]
    public ActionResult GetRangeInfo(IRangesProvider rangesProvider, string name)
    {

        var data = rangesProvider.Find(new DataName(name));
        if(data == null)
        {
            return NotFound($"Data '{name}' not found");
        }

        return Ok(GetRangesText([data]));
    }

    private string GetRangesText(IReadOnlyCollection<Data> datas)
    {
        var nl = Constants.NewLine;

        return string.Join(Constants.NewLine, datas.Select(data => "-------- " + data.Name
                                                                               + nl + nl +
                                                                               data.Info
                                                                               + nl + nl + nl + nl));
    }
}