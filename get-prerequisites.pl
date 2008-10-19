use strict;
use File::Spec;
use File::Path;
use LWP::Simple;
use Archive::Extract;

my $libDir = File::Spec->catdir($0, "..", "..", "lib");
my $cacheDir = File::Spec->catdir($ENV{TEMP}, 'get-req-cache');
mkpath $libDir;

downloadAndExtract("http://dfn.dl.sourceforge.net/sourceforge/sqlite-dotnet2/SQLite-1.0.60.0-binaries.zip", $libDir, 'SQLite');
downloadAndExtract("http://ovh.dl.sourceforge.net/sourceforge/sqlite-dotnet2/SQLite-1.0.60.0-source.zip", $libDir, 'SQLite');
downloadAndExtract("http://www.componentace.com/data/distr/zlib.NET_104.zip", $libDir, 'zlib.NET_104');
downloadAndExtract("http://kent.dl.sourceforge.net/sourceforge/sharpdevelop/SharpZipLib_0855_Bin.zip", $libDir, 'SharpZipLib_0855');
downloadAndExtract("http://archive.apache.org/dist/incubator/log4net/1.2.10/incubating-log4net-1.2.10.zip", $libDir, '.');
downloadAndExtract("http://kent.dl.sourceforge.net/sourceforge/nunit/NUnit-2.4.8-net-2.0.zip", $libDir, 'NUnit-2.4.8-net-2.0');
# rar ?

sub download
{
    my ($url, $downloadDir) = @_;
    print "$url\n";
    my @p = split /\//, $url;
    my $filename = File::Spec->catdir($downloadDir, pop @p);
    mkpath $downloadDir;
    mirror($url, $filename);
    return $filename;
}

sub downloadAndExtract
{
    my ($url, $dir, $prefix) = @_;
    my $archive = download($url, $cacheDir);
    my $ae = Archive::Extract->new( archive => $archive );
    $ae->extract( to => File::Spec->catdir($dir, $prefix) );
}
