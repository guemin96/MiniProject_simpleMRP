SELECT * FROM Schedules

SELECT * FROM process

SELECT s.*,p.* FROM Schedules AS s
INNER JOIN Process AS p
ON s.SchIdx = p.SchIdx

-- 1. PrcResult에서 성공개수와 실패 개수를 다른 (가상) 컬럼으로 분리, ELSE를 안넣을 경우 null값이 들어감
SELECT p.SchIdx,p.PrcDate,
	   CASE p.PrcResult WHEN 1 THEN 1 ELSE 0 END AS PrcOK,
	   CASE p.prcResult WHEN 0 THEN 1 ELSE 0 END AS PrcFail
  FROM Process AS p

-- 2. 합계집계 결과 가상테이블 
SELECT smr.SchIdx,smr.PrcDate,
	   count(smr.PrcOK) as PrcOKAmount,count(smr.PrcFail) as PrcFailAmount
  FROM (
		SELECT p.SchIdx,p.PrcDate,
			CASE p.PrcResult WHEN 1 THEN 1 END AS PrcOK,
			CASE p.prcResult WHEN 0 THEN 1 END AS PrcFail
		  FROM Process AS p
	   ) AS smr
 GROUP BY smr.SchIdx,smr.PrcDate

 -- 3.0 조인문
 SELECT *FROM Schedules AS sch INNER JOIN Process AS prc
 ON sch.SchIdx = prc.SchIdx

 -- 3.1 2번결과(가상 테이블)와 schedules 테이블 조인해서 원하는 결과 도출
 SELECT sch.SchIdx,sch.PlantCode, sch.SchAmount, prc.PrcDate,
		prc.PrcOKAmount,prc.PrcFailAmount
   FROM Schedules AS sch
  INNER JOIN (
		SELECT smr.SchIdx,smr.PrcDate,
			count(smr.PrcOK) as PrcOKAmount,count(smr.PrcFail) as PrcFailAmount
		FROM (
			SELECT p.SchIdx,p.PrcDate,
				CASE p.PrcResult WHEN 1 THEN 1 END AS PrcOK,
				CASE p.prcResult WHEN 0 THEN 1 END AS PrcFail
				FROM Process AS p
			) AS smr
		GROUP BY smr.SchIdx,smr.PrcDate
  )AS prc
  ON sch.SchIdx = prc.SchIdx


