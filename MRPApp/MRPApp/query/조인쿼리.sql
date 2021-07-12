SELECT * FROM Schedules

SELECT * FROM process

SELECT s.*,p.* FROM Schedules AS s
INNER JOIN Process AS p
ON s.SchIdx = p.SchIdx

-- 1. PrcResult���� ���������� ���� ������ �ٸ� (����) �÷����� �и�, ELSE�� �ȳ��� ��� null���� ��
SELECT p.SchIdx,p.PrcDate,
	   CASE p.PrcResult WHEN 1 THEN 1 ELSE 0 END AS PrcOK,
	   CASE p.prcResult WHEN 0 THEN 1 ELSE 0 END AS PrcFail
  FROM Process AS p

-- 2. �հ����� ��� �������̺� 
SELECT smr.SchIdx,smr.PrcDate,
	   count(smr.PrcOK) as PrcOKAmount,count(smr.PrcFail) as PrcFailAmount
  FROM (
		SELECT p.SchIdx,p.PrcDate,
			CASE p.PrcResult WHEN 1 THEN 1 END AS PrcOK,
			CASE p.prcResult WHEN 0 THEN 1 END AS PrcFail
		  FROM Process AS p
	   ) AS smr
 GROUP BY smr.SchIdx,smr.PrcDate

 -- 3.0 ���ι�
 SELECT *FROM Schedules AS sch INNER JOIN Process AS prc
 ON sch.SchIdx = prc.SchIdx

 -- 3.1 2�����(���� ���̺�)�� schedules ���̺� �����ؼ� ���ϴ� ��� ����
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


