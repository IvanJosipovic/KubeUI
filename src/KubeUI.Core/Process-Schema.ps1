Add-Type -AssemblyName System.Web
$filename = "Models\Schema.cs";

$contents = Get-Content -Path $filename;
$newContents = "";

foreach($line in $contents){
    if($line.Contains("[System.CodeDom.Compiler.GeneratedCode")){
	#} elseif ($line.Contains("[Newtonsoft.Json.JsonProperty(")){
		#$padding = $line.SubString(0, $line.IndexOf("[Newtonsoft"));
		#$part = $line.Trim().Replace("[Newtonsoft.Json.JsonProperty(`"","");
		#$part = $part.SubString(0, $part.IndexOf("`""))
        #$newContents += $padding + "[System.Text.Json.Serialization.JsonPropertyName(`"" + $part + "`")]" + [System.Environment]::NewLine;
    } else {
        $newContents += $line + [System.Environment]::NewLine;
    }
}

$newContents | Out-File -FilePath $filename;