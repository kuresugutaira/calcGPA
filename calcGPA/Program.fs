open System
open canopy.runner.classic
open canopy.configuration
open canopy.classic

canopy.configuration.chromeDir <- System.AppContext.BaseDirectory

//Twinsログイン用にユーザ名とパスワードを取得
Console.WriteLine "Twinsのユーザ名を入力してね。"
let userName = Console.ReadLine()
Console.WriteLine "Twinsのパスワードを入力してね。"
let pass = Console.ReadLine()

//chromeインスタンスを生成
start chrome

onFail (fun _ ->
printfn "Twinsにログインできないみたい、ユーザ名とパスワードがあってるか確認してみて！"
printfn "それでも直らなかったらTwitter : @tsuGoojiまで連絡してね！！"
)

"起動するよ" &&& fun _ ->

    //urlのページを開く
    url "https://twins.tsukuba.ac.jp/campusweb/campusportal.do"
    
    //Twinsでユーザ名とパスワードを打ち込んでログインする
    "#LoginFormSlim > tbody > tr > td:nth-child(3) > input[type=text]" << userName
    "#LoginFormSlim > tbody > tr > td:nth-child(5) > input[type=password]" << pass
    click "#LoginFormSlim > tbody > tr > td:nth-child(6) > button > span"
    
    sleep 3 //ログイン時間を考慮して3秒待つ

    displayed "#tab-home > img"

    click "#tab-si > img"

    "#rishuSeisekiReferListForm > select" << "200"
    click "#rishuSeisekiReferListForm > input.ui-button.ui-widget.ui-state-default.ui-corner-all"

    let tmp = (read "#rishuSeisekiReferListForm > b:nth-child(8)").ToCharArray() //「n件目」の文字列を取得
    let nS = Array.sub tmp 0 ((Array.length tmp) - 2)
    let n = String(nS) |> int //要素数を取得

    //xは番号,yはリスト,(単位数,評価,GPA計算対象)のタプルのリストを作ってくれる
    //例 : (2.0,A,true) <- 2単位で評価がA、GPA計算対象内である科目のデータ
    let rec makeDataLists (x,y) = 
        if x > n then y 
        else
        let xPulsOne = string (x + 1)
        let item = read ("#rishuSeisekiReferListForm > table.normal > tbody > tr:nth-child(" + xPulsOne + ")")
        let itemList = item.Split(' ')
        let isGPAtaisyou str = 
            match str with 
                | "GPA計算対象外科目" -> false
                | _ -> true
        let taisyou = isGPAtaisyou itemList.[4]
        let dataList = (string (itemList.[(Array.length itemList) - 4]),string (itemList.[(Array.length itemList) - 1]),taisyou)
        makeDataLists (x + 1,dataList::y)

    //ABCD評価をスコアに変換したりGPA計算対象外を除外したりする
    let trim (x,y,z) = 
        match (y,z) with
           | ("A+",true) -> (float x,(float x) * 4.3)
           | ("A",true) -> (float x,(float x) * 4.0)
           | ("B",true) -> (float x,(float x) * 3.0) 
           | ("C",true) -> (float x,(float x) * 2.0)
           | ("D",true) -> (float x,0.0)
           | _ -> (0.0,0.0)

    //データリストからGPAを計算してくれる関数
    let calcGPA (x,y) = 
        (List.sumBy float x,List.sum y)
        |> (fun (u,v) -> v / u)
        |> string

    //GPAを計算する
    let gpa = makeDataLists (1,[]) |> List.map trim |> List.unzip |> calcGPA

    Console.WriteLine ("あなたのGPAは\n" + gpa + "\nだよ。")

    let hitokoto gpa = 
        match gpa with
            | _ when gpa < 1. -> "これは...すごいね、ある意味才能かも..."
            | _ when gpa < 2. -> "今回できなくても次できればokだから頑張ろう！"
            | _ when gpa < 3. -> "悪くないよ、3.0以上目指してファイト！"
            | _ when gpa < 4. -> "おお、良い感じだね！目標を高くもって気合い入れていこう！"
            | _ when gpa < 4.3 -> "すごい、とても優秀なんだね！維持できるように応援してるよ！"
            | _ when gpa = 4.3 -> "て、天才...?将来ノーベル賞とれちゃうかも..."
            | _ -> "本来ならこうはならないgpaになってるね...バグってるみたいだからTwitter : @tsuGoojiまで連絡してね！"

    Console.WriteLine (hitokoto (float gpa))

    click "#logout > li:nth-child(1)"

    click "#logoutCLOSE"

//プログラムを走らせる
run()

printfn "Enterキーを押してプログラムを終了してね！"
System.Console.ReadLine() |> ignore

quit()