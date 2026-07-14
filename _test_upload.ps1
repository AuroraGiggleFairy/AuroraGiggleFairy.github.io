param([string]$url)
try {
  $bytes = [byte[]]@(0x41 * 100)
  $req = [System.Net.HttpWebRequest]::Create($url)
  $req.Method = 'PUT'
  $req.ContentType = 'application/octet-stream'
  $req.Headers['Content-Disposition'] = 'attachment; filename="test.txt"'
  $req.ContentLength = $bytes.Length
  $req.AutomaticDecompression = [System.Net.DecompressionMethods]::None
  $req.Accept = $null
  $req.UserAgent = $null
  [System.Net.ServicePointManager]::Expect100Continue = $false
  $stream = $req.GetRequestStream()
  $stream.Write($bytes, 0, $bytes.Length)
  $stream.Close()
  $resp = $req.GetResponse()
  Write-Output ('HTTP ' + [int]$resp.StatusCode)
  $resp.Close()
} catch [System.Net.WebException] {
  try { $resp = $_.Exception.Response; $s = 'HTTP ' + [int]$resp.StatusCode; $body = [System.IO.StreamReader]::new($resp.GetResponseStream()).ReadToEnd(); Write-Output ('ERROR:' + $s); Write-Output ($body.Substring(0, [Math]::Min(200, $body.Length))) } catch { Write-Output ('ERROR: ' + $_.Exception.Message) }
} catch { Write-Output ('ERROR: ' + $_.Exception.Message) }