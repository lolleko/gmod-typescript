package main

import (
	"encoding/base64"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"net/http"
	"net/url"
	"os"
	"path"
	"strconv"
	"strings"
	"time"
)

type categoryJSON struct {
	PageID int    `json:"pageid"`
	Title  string `json:"title"`
}

type innerQueryJSON struct {
	CategoryMembers []categoryJSON `json:"categorymembers"`
}

type categoryQueryJSON struct {
	Query innerQueryJSON `json:"query"`
}

func savePageBody(page string, raw bool) []byte {
	page = strings.Replace(page, " ", "_", -1)
	base := "https://wiki.garrysmod.com/"
	if raw {
		page = "page/" + strings.Replace(page, ".", "%2E", -1) + "?action=raw"
	}
	rs, err := http.Get(base + page)
	if err != nil {
		panic(err)
	}
	defer rs.Body.Close()

	if rs.StatusCode != http.StatusOK {
		fmt.Println("Error Code: " + strconv.Itoa(rs.StatusCode) + " " + page)
		panic(rs.StatusCode)
	}

	bodyBytes, err := ioutil.ReadAll(rs.Body)
	if err != nil {
		panic(err)
	}

	fileName := base64.StdEncoding.EncodeToString([]byte(url.QueryEscape(page)))

	err = ioutil.WriteFile("../wikiData/"+fileName, bodyBytes, 0644)
	if err != nil {
		panic(err)
	}

	fmt.Println("Downloaded: " + page)

	return bodyBytes
}

func savePagesInCategory(category string) categoryQueryJSON {
	queryBody := savePageBody("api.php?action=query&list=categorymembers&cmtitle=Category:"+category+"&cmlimit=10000&format=json", false)
	go savePageBody("Category:"+category, true)

	var result categoryQueryJSON
	err := json.Unmarshal(queryBody, &result)
	if err != nil {
		fmt.Println(category, string(queryBody))
		panic(err)
	}

	rate := time.Second / 50
	throttle := time.Tick(rate)
	for _, member := range result.Query.CategoryMembers {
		<-throttle
		go savePageBody(member.Title, true)
	}

	return result
}

func main() {
	os.Mkdir("../wikiData", os.ModePerm)
	categories := []string{
		"Enumerations",
		"Global",
		"Structures",
	}

	for _, category := range categories {
		savePagesInCategory(category)
	}

	functionsAndCategory := []string{
		"Hooks",
		"Class_Functions",
		"Library_Functions",
	}

	for _, category := range functionsAndCategory {
		catQuery := savePagesInCategory(category)
		alreadySaved := map[string]bool{}
		for _, member := range catQuery.Query.CategoryMembers {
			subCat := path.Dir(member.Title)
			if !alreadySaved[subCat] {
				alreadySaved[subCat] = true
				if category == "Hooks" {
					subCat = subCat + "_Hooks"
				}
				savePageBody("Category:"+subCat, true)
			}
		}
	}

	categoryAndSubCats := []string{
		"Panels",
	}

	for _, category := range categoryAndSubCats {
		catQuery := savePagesInCategory(category)
		for _, member := range catQuery.Query.CategoryMembers {
			savePagesInCategory(strings.Replace(member.Title, "Category:", "", 1))
		}
	}
}
