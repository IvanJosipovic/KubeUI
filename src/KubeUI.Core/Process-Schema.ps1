Add-Type -AssemblyName System.Web
$filename = "Models\Schema.cs";

$contents = Get-Content -Path $filename;
$newContens = "";

foreach($line in $contents){
    if($line.Contains("[System.CodeDom.Compiler.GeneratedCode")){
        #Write-Output $line;
    } else {
        $newContens += $line;
        $newContens += [System.Environment]::NewLine;
    }
}

$newContens | Out-File -FilePath $filename;