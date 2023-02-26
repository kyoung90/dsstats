
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using dsstats.mmr.ProcessData;
using pax.dsstats.shared;

namespace dsstats.mmr;

public partial class MmrService
{
    private static readonly List<int> possiblecmdrs = new() { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170 };
    private static readonly string tfurl = "http://localhost:8501/v1/models/dsstatsModel:predict";
    private static readonly string tfResturl = "/v1/models/dsstatsModel:predict";
    private static readonly int tfPort = 8501;

    public static async Task<double> GetTeam1ExpectationToWinFromTf(ReplayData replayData)
    {
        int[] cmdrData = GetCmdrData(replayData.ReplayDsRDto);
        float[] ratingData = GetRatingData(replayData);

        TfPayload tfPayload = GetPayload(cmdrData, ratingData);

        // double team1ExpectationToWin = await GetTfResult(tfPayload);
        double team1ExpectationToWin = await GetTfResult2(tfPayload);
        return team1ExpectationToWin;
    }

    private static async Task<double> GetTfResult2(TfPayload playload)
    {
        var data = JsonSerializer.Serialize(playload);

        var content = new StringContent(data, Encoding.UTF8, "application/json");
        var response = await new HttpClient().PostAsync(tfurl, content);

        if (!response.IsSuccessStatusCode)
        {
            return 0.5;
        }

        var responseString = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<TfResponse>(responseString);
        if (result != null && result.Outputs.Length > 0 && result.Outputs[0].Length > 0)
        {
            return result.Outputs[0][0];
        }
        return 0.5;
    }

    private static async Task<double> GetTfResult(TfPayload tfPayload, int timeout = 1000)
    {
        var data = JsonSerializer.Serialize(tfPayload);
        var n = data.Length;

        StringBuilder sb = new();
        sb.AppendLine($"POST {tfResturl} HTTP/1.1");
        sb.AppendLine("Host: pax77.dsstats.org");
        sb.AppendLine("Accept: application/json");
        sb.AppendLine("Content-Type: application/json");
        sb.AppendLine($"Content-Length: {n}");
        sb.AppendLine();
        sb.AppendLine(data);

        using TcpClient client = new TcpClient();

        await client.ConnectAsync("localhost", tfPort);

        using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        
        writer.AutoFlush = true;
        stream.ReadTimeout = timeout;

        await writer.WriteLineAsync(sb.ToString());
        var response = await reader.ReadToEndAsync();

        var result = JsonSerializer.Deserialize<TfResponse>(response);
        if (result != null && result.Outputs.Length > 0 && result.Outputs[0].Length > 0)
        {
            return result.Outputs[0][0];
        }
        return 0.5;
    }

    private static TfPayload GetPayload(int[] cmdrData, float[] ratingData)
    {
        var tfCmdrData = new int[1][][];
        tfCmdrData[0] = new int[possiblecmdrs.Count * 2][];
        for (int i = 0; i < tfCmdrData[0].Length; i++)
        {
            tfCmdrData[0][i] = new int[1];
            tfCmdrData[0][i][0] = cmdrData[i];
        }

        float[] normalizedRatingData = GetNormalizedRatings(ratingData);
        var tfRatingData = new float[1][][];
        tfRatingData[0] = new float[6][];

        for (int i = 0; i < normalizedRatingData.Length; i++)
        {
            tfRatingData[0][i] = new float[1];
            tfRatingData[0][i][0] = normalizedRatingData[i];
        }

        return new()
        {
            Inputs = new()
            {
                CmdrsInput = tfCmdrData,
                RatingsInput = tfRatingData
            }
        };
    }

    private static float[] GetNormalizedRatings(float[] ratingData, float min = 0, float max = 3000)
    {
        var result = new float[ratingData.Length];
        for (int i = 0; i < ratingData.Length; i++)
        {
            result[i] = (ratingData[i] - min) / (max - min);
        }
        return result;
    }

    private static int[] GetCmdrData(ReplayDsRDto replayDsRDto)
    {
        int[] cmdrData = new int[possiblecmdrs.Count * 2];
        Array.Clear(cmdrData, 0, cmdrData.Length);

        int i = 0;
        foreach (var player in replayDsRDto.ReplayPlayers.OrderBy(o => o.GamePos))
        {
            // if (replay.Duration - player.Duration > 89)
            // {

            // }
            var commander = player.Race;
            var commanderIndex = possiblecmdrs.IndexOf((int)commander);
            
            // fs - todo!
            if (commanderIndex < 0)
            {
                commanderIndex = 0;
            }
            
            if (player.Team == 1)
            {
                cmdrData[commanderIndex] = 1;
            }
            else
            {
                cmdrData[commanderIndex + possiblecmdrs.Count] = 1;
            }
            i++;
        }
        return cmdrData;
    }

    private static float[] GetRatingData(ReplayData replayData)
    {
        float[] ratingData = new float[6];

        if (replayData.ReplayDsRDto.WinnerTeam == 1)
        {
            for (int t1 = 0; t1 < replayData.WinnerTeamData.Players.Length; t1++)
            {
                ratingData[t1] = (float)replayData.WinnerTeamData.Players[t1].Mmr;
            }
            for (int t2 = 0; t2 < replayData.LoserTeamData.Players.Length; t2++)
            {
                ratingData[t2 + replayData.WinnerTeamData.Players.Length] = (float)replayData.LoserTeamData.Players[t2].Mmr;
            }
        }
        else
        {
            for (int t1 = 0; t1 < replayData.LoserTeamData.Players.Length; t1++)
            {
                ratingData[t1] = (float)replayData.LoserTeamData.Players[t1].Mmr;
            }
            for (int t2 = 0; t2 < replayData.WinnerTeamData.Players.Length; t2++)
            {
                ratingData[t2 + replayData.LoserTeamData.Players.Length] = (float)replayData.WinnerTeamData.Players[t2].Mmr;
            }
        }
        return ratingData;
    }
}

internal record TfPayload
{
    [JsonPropertyName("signature_name")]
    public string SignatureName { get; set; } = "serving_default";

    [JsonPropertyName("inputs")]
    public TfPayloadInputs Inputs { get; set; } = null!;
}
internal record TfPayloadInputs
{
    [JsonPropertyName("cmdrs_input")]
    public int[][][] CmdrsInput { get; set; } = null!;

    [JsonPropertyName("ratings_input")]
    public float[][][] RatingsInput { get; set; } = null!;
}

internal record TfResponse
{
    [JsonPropertyName("outputs")]
    public float[][] Outputs { get; set; } = null!;
}