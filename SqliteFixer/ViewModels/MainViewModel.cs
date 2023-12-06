using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace SqliteFixer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
	private static string SqliteToolsDownloadLink => "https://raw.githubusercontent.com/ElectronicObserverEN/SqliteFixer/main/sqlite3.exe";
	private static string SqliteToolsPath => "sqlite3.exe";
	private static string DbFilePath => "Record/ElectronicObserver.sqlite";

	public ObservableCollection<string> Logs { get; } = new();

	[RelayCommand]
	private async Task StartRepair()
	{
		try
		{
			if (!File.Exists(SqliteToolsPath))
			{
				await DownloadSqliteTools();
			}

			string backupFilePath = Path.ChangeExtension(DbFilePath, DateTime.UtcNow.ToString("yyyyMMdd_HHmmss"));

			File.Move(DbFilePath, backupFilePath);

			string arguments = $"""/C {SqliteToolsPath} {backupFilePath} ".recover" | {SqliteToolsPath} {DbFilePath}""";

			await Process.Start("cmd", arguments).WaitForExitAsync();

			AddLog("Finished fixing sqlite file.");
		}
		catch (Exception e)
		{
			AddLog("Failed to fix sqlite file", e);
		}
	}

	private async Task DownloadSqliteTools()
	{
		try
		{
			HttpClient client = new();

			await using Stream toolsStream = await client.GetStreamAsync(SqliteToolsDownloadLink);

			await using FileStream file = File.Create(SqliteToolsPath);
			await toolsStream.CopyToAsync(file);

			AddLog("Finished downloading sqlite tools.");
		}
		catch
		{
			AddLog("Failed to download sqlite tools");
			throw;
		}
	}

	private void AddLog(string message, Exception? e = null)
	{
		if (e is not null)
		{
			message += $" {e.GetBaseException().Message} {e.StackTrace}";
		}

		Logs.Add($"[{DateTime.Now:yyyy/MM/dd HH:mm:ss}]: {message}");
	}
}
