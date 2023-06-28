﻿using Microsoft.AspNetCore.Components;
using pax.dsstats.shared;
using pax.BlazorChartJs;
using Microsoft.JSInterop;
using static sc2dsstats.razorlib.Stats.StatsChartComponent;
using sc2dsstats.razorlib.Services;

namespace sc2dsstats.razorlib.Stats.Winrate;

public partial class WinrateChart : ComponentBase
{
    [Parameter, EditorRequired]
    public WinrateResponse Response { get; set; } = default!;

    [Parameter, EditorRequired]
    public WinrateRequest Request { get; set; } = default!;

    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    IconsChartJsConfig chartConfig = null!;
    bool chartReady;
    bool iconsReady;
    private int iconX = 30;
    private int iconY = 30;

    ChartComponent? chartComponent;
    private readonly string mainColor = "#3F5FFA";

    protected override void OnInitialized()
    {
        chartConfig = GetChartConfig();
        base.OnInitialized();
    }

    private void ChartEventTriggered(ChartJsEvent chartEvent)
    {
        if (chartEvent is ChartJsInitEvent initEvent)
        {
            chartReady = true;

            if (!iconsReady)
            {
                JSRuntime.InvokeVoidAsync("registerImagePlugin", iconX, iconY);
                JSRuntime.InvokeVoidAsync("increaseChartHeight", chartConfig.ChartJsConfigGuid, iconY);
                iconsReady = true;
            }

            PrepareData(Response, Request);
        }
    }

    public void PrepareData(WinrateResponse response, WinrateRequest request)
    {
        if (!chartReady)
        {
            return;
        }

        Response = response;
        Request = request;

        if (chartConfig.Data.Datasets.Any())
        {
            chartConfig.RemoveDatasets(chartConfig.Data.Datasets);
        }

        if (chartConfig.Options?.Plugins?.Title != null)
        {
            if (request.Interest == Commander.None)
            {
                chartConfig.Options.Plugins.Title.Text = new IndexableOption<string>($"Average rating gain - {Data.GetTimePeriodLongName(request.TimePeriod)}");
            } else
            {
                chartConfig.Options.Plugins.Title.Text = new IndexableOption<string>($"{request.Interest}'s average rating gain - {Data.GetTimePeriodLongName(request.TimePeriod)}");
                //chartConfig.Options.Plugins.Title.Text =
                //    new IndexableOption<string>(new List<string>()
                //    {
                //        $"Average rating gain - {Data.GetTimePeriodLongName(request.TimePeriod)}",
                //        $"for {request.Interest} vs"
                //    });
            }
            chartConfig.UpdateChartOptions();
        }

        chartConfig.SetLabels(response.WinrateEnts.Select(s => s.Commander.ToString()).ToList());

        chartConfig.AddDataset(GetAvgGainDataset(response));
        // chartConfig.AddDataset(GetWinrateDataset(response));

        SetIcons(response);
        JSRuntime.InvokeVoidAsync("setDatalabelsFormatter", chartConfig.ChartJsConfigGuid);
    }

    private ChartJsDataset GetAvgGainDataset(WinrateResponse response)
    {
        var data = response.WinrateEnts.Select(s => s.AvgGain).Cast<object>().ToList();

        var barDataset = new BarDataset()
        {
            Label = $"AvgGain",
            Data = data,
            BackgroundColor = new IndexableOption<string>(response.WinrateEnts.Select(s => Data.GetBackgroundColor(s.Commander)).ToList()),
            BorderColor = new IndexableOption<string>(response.WinrateEnts.Select(s => Data.CmdrColor[s.Commander]).ToList()),
            BorderWidth = new IndexableOption<double>(2),
            // Stack = "Stack 0"
        };

        return barDataset;
    }

    private ChartJsDataset GetWinrateDataset(WinrateResponse response)
    {
        var data = response.WinrateEnts.Select(s => s.Count == 0 ? 0 : Math.Round((double)s.Wins / s.Count, 2)).Cast<object>().ToList();

        var barDataset = new BarDataset()
        {
            Label = $"Winrate",
            Data = data,
            BackgroundColor = response.Interest == Commander.None ?
                new IndexableOption<string>(response.WinrateEnts.Select(s => Data.GetBackgroundColor(s.Commander)).ToList())
                : new IndexableOption<string>(Data.GetBackgroundColor(response.Interest)),
            BorderColor = response.Interest == Commander.None ?
                new IndexableOption<string>(response.WinrateEnts.Select(s => Data.CmdrColor[s.Commander]).ToList())
                : new IndexableOption<string>(Data.CmdrColor[response.Interest]),
            BorderWidth = new IndexableOption<double>(2),
            Stack = "Stack 1"
        };

        return barDataset;
    }

    private void SetIcons(WinrateResponse response)
    {
        if (chartConfig.Options?.Plugins != null 
            && chartConfig.Options?.Plugins is IconsPlugins iconPlugins)
        {
            var icons = response.WinrateEnts.Select(s => new ChartIconsConfig()
            {
                XWidth = iconX,
                YWidth = iconY,
                // YOffset = s.AvgGain < 0 ? iconY : 0,
                YOffset = 0,
                ImageSrc = HelperService.GetImageSrc(s.Commander),
                Cmdr = s.Commander.ToString().ToLower()
            }).ToList();

            iconPlugins.BarIcons = icons;
            chartConfig.UpdateChartOptions();
        }
    }

    private IconsChartJsConfig GetChartConfig()
    {
        return new IconsChartJsConfig()
        {
            Type = ChartType.bar,
            Options = new IconsChartJsOptions()
            {
                MaintainAspectRatio = true,
                Responsive = true,
                OnClickEvent = true,
                Plugins = new IconsPlugins()
                {
                    Title = new()
                    {
                        Display = true,
                        Text = new IndexableOption<string>("Winrate"),
                        Color = "white",
                        Font = new()
                        {
                            Size = 16,
                        }
                    },
                    Datalabels = new()
                    {
                        Display = "auto",
                        Color = "#0a050c",
                        BackgroundColor = "#cdc7ce",
                        BorderColor = "#491756",
                        BorderRadius = 4,
                        BorderWidth = 1,
                        Anchor = "end",
                        Align = "start",
                        Clip = true
                    },
                    Legend = new Legend()
                    {
                        Display = false,
                        Labels = new Labels()
                        {
                            Padding = 0,
                            BoxHeight = 0,
                            BoxWidth = 0
                        }
                    }
                },
                Scales = new()
                {
                    X = new LinearAxis()
                    {
                        // Stacked = true,
                        Ticks = new ChartJsAxisTick()
                        {
                            Color = mainColor
                        },
                        Grid = new ChartJsGrid()
                        {
                            Display = true,
                            Color = "rgba(113, 116, 143, 0.25)",
                            TickColor = "rgba(113, 116, 143, 0.75)",
                            Z = -1
                        },
                        Border = new ChartJsAxisBorder()
                        {
                            Display = true,
                            Color = "rgba(113, 116, 143)",
                            Dash = new List<double>() { 2, 4 }
                        }
                    },
                    Y = new LinearAxis()
                    {
                        // Stacked = true  
                        Title = new Title()
                        {
                            Display = true,
                            Text = new IndexableOption<string>("Average rating gain"),
                            Color = mainColor
                        },
                        Ticks = new ChartJsAxisTick()
                        {
                            Color = mainColor
                        },
                        Grid = new ChartJsGrid()
                        {
                            Display = true,
                            Color = "rgba(113, 116, 143, 0.25)",
                            TickColor = "rgba(113, 116, 143, 0.75)",
                            Z = -1
                        },
                        Border = new ChartJsAxisBorder()
                        {
                            Display = true,
                            Color = "rgba(113, 116, 143)",
                            Dash = new List<double>() { 2, 4 }
                        }
                    }
                }
            }
        };
    }
}
