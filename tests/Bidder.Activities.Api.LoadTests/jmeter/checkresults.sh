statisticsFile=$1
testperformancethreshold=$2

getErrorCount(){
  local statisticsFile=$1
  cat "$statisticsFile" | jq '.[].errorCount' | uniq | sort -nr | head -n1
}

getSecondPercentileCount(){
  local statisticsFile=$1
  cat "$statisticsFile" | jq '.[].pct2ResTime' | uniq | sort -nr | head -n1
}

echo 'Extracting test results'

cat $statisticsFile

errors=$(getErrorCount $statisticsFile)
secondPercentile=$(getSecondPercentileCount $statisticsFile)

echo  "$secondPercentile"
echo  "$testperformancethreshold"

 if [ "$errors" != "0" ];then
      echo "Test ended with errors"
      exit 125    
    else
          echo "Test has no errors" 
          cond=$(awk 'BEGIN {print ('$secondPercentile' >= '$testperformancethreshold')}')       
           if [ "$cond" == "1" ]
          then
               echo "Test second percentile above threshold"
               exit 125
          fi
  fi